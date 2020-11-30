#ifndef SERVER_CONNECTION_h
#define SERVER_CONNECTION_h
#pragma once

#include <EstablishPayload.h>
#include <Logger.h>
#include <ESP8266WiFi.h>
#include <PayloadHeader.h>
#include <Status.h>

class ServerConnection
{
public:
    ServerConnection(Logger *logger);
    void begin(const char *ssid, const char *passphrase, const char *server = nullptr);
    void run();
    void updateSensor(uint8_t id, uint8_t value);
    void ping();
    void updateStatus(Status status);
    unsigned int latency();

private:
    bool tryReadPayload();

    void fatal(const char *message);
    void processPayload(unsigned char op_code, unsigned char *payload_buffer, size_t length);

    template <typename T>
    void sendPayload(unsigned char op_code, T *payload, size_t length);

    template <typename T>
    bool map(unsigned char *payload_buffer, size_t length, T *&payload);

    void doConnect(const char *server);

    float _latency;
    Logger *_logger;
    unsigned long _last_ping_time;
    unsigned char _pending_pings;
    WiFiClient _wifi_client;
    PayloadHeader _previous_header;
    bool _previous_header_processed;
};

#endif // SERVER_CONNECTION_h