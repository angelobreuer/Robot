#include <ServerConnection.h>
#include <ESP8266WiFi.h>
#include <EstablishPayload.h>
#include <SensorSyncPayload.h>
#include <StatusPayload.h>

ServerConnection::ServerConnection(Logger *logger) : _logger(logger), _last_ping_time(0), _pending_pings(0)
{
}

void ServerConnection::begin(const char *ssid, const char *passphrase, const char *server)
{
    _logger->write("Starting connection...");
    _logger->writef("Connecting to %s...", ssid);

    WiFi.begin(ssid, passphrase);

    unsigned long startTime = millis();

    while (WiFi.status() != WL_CONNECTED && millis() < startTime + 60 * 1000)
    {
        delay(50);
    }

    if (WiFi.status() != WL_CONNECTED)
    {
        _logger->writef("Connection to %s failed: %d!", ssid, WiFi.status());
        fatal("WiFi connection failed.");
    }

    unsigned char ipAddr[40];
    WiFi.localIP().toString().getBytes(ipAddr, 40);

    _logger->writef("Connected to %s, ip address: %s.", ssid, ipAddr);

    if (!server)
    {
        IPAddress gatewayIpAddr = WiFi.gatewayIP();
        gatewayIpAddr.toString().getBytes(ipAddr, 40);
        doConnect((const char *)ipAddr);
    }
    else
    {
        doConnect(server);
    }

    char buffer[6];
    buffer[0] = (char)'R';
    buffer[1] = (char)'b';
    buffer[2] = (char)'t';
    buffer[3] = (char)'C';
    buffer[4] = (char)'l';
    buffer[5] = (char)'t';
    sendPayload(0, buffer, 6);
}

void ServerConnection::ping()
{
    if (++_pending_pings > 5)
    {
        fatal("Server did not acknowledge the last 5 pings.");
    }

    sendPayload<void>(2, nullptr, 0);
}

void ServerConnection::processPayload(unsigned char op_code, unsigned char *payload_buffer, size_t length)
{
    if (op_code == 0x3)
    {
        if (--_pending_pings < 0)
        {
            fatal("A ping was acknowledged that was not sent.");
        }

        _latency = ((float)(micros64() - _last_ping_time)) / (float)1000;
        _logger->writef("Received pong. Latency: %.2fms.", _latency);
    }
}

void ServerConnection::updateStatus(Status status)
{
    StatusPayload payload;
    payload.status = status;

    sendPayload(4, &payload, 1);
}

void ServerConnection::updateSensor(uint8_t id, uint8_t value)
{
    SensorSyncPayload payload;
    payload.id = id;
    payload.value = value;

    sendPayload(1, &payload, 2);
}

void ServerConnection::doConnect(const char *server)
{
    _logger->writef("Connecting to server: %s...", server);

    if (!_wifi_client.connect(server, 8080) || !_wifi_client.connected())
    {
        fatal("Failed to connect to server");
    }
}

void ServerConnection::fatal(const char *message)
{
    _logger->writef("[FATAL] %s, restarting.", message);
    ESP.restart();

    while (true)
    {
        yield();
    }
}

template <typename T>
void ServerConnection::sendPayload(unsigned char op_code, T *payload, size_t payload_length)
{
    char payload_buffer[payload_length + 3];
    PayloadHeader *header = (PayloadHeader *)&payload_buffer;

    // clear header space
    memset(payload_buffer, 0, 3);

    header->op_code = op_code;
    header->length = payload_length;

    memcpy(&payload_buffer[3], (void *)payload, payload_length);

    _wifi_client.write((const char *)(&payload_buffer), payload_length + 3);
    _logger->writef("Sent payload %d (%d bytes).", op_code, payload_length);
}

bool ServerConnection::tryReadPayload()
{
    if (_previous_header_processed)
    {
        return true;
    }

    if (_wifi_client.available() < _previous_header.length)
    {
        return false;
    }

    byte payload_buffer[_previous_header.length];
    _wifi_client.read(payload_buffer, _previous_header.length);
    _logger->writef("Received payload %d (%d bytes).", _previous_header.op_code, _previous_header.length);

    processPayload(_previous_header.op_code, payload_buffer, _previous_header.length);
    return true;
}

template <typename T>
bool ServerConnection::map(unsigned char *payload_buffer, size_t length, T *&payload)
{
    if (length < sizeof(T))
    {
        return false;
    }

    payload = ((T *)&payload_buffer);
    return true;
}

void ServerConnection::run()
{
    if (micros64() >= _last_ping_time + 2000000UL)
    {
        ping();
        _last_ping_time = micros64();
    }

    if (!_previous_header_processed)
    {
        if (!tryReadPayload())
        {
            return;
        }

        _previous_header_processed = true;
    }

    while (_wifi_client.available() > 3)
    {
        unsigned char header_buffer[3];

        if (!_wifi_client.read(header_buffer, 3))
        {
            fatal("Receive error.");
        }

        _previous_header.op_code = header_buffer[0];
        _previous_header.length = *((uint16_t *)(((void *)header_buffer) + 1));
        _previous_header_processed = false;

        if (!tryReadPayload())
        {
            return;
        }

        _previous_header_processed = true;
    }
}