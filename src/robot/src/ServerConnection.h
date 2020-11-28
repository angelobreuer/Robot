#ifndef SERVER_CONNECTION_h
#define SERVER_CONNECTION_h
#pragma once

#include <EstablishPayload.h>
#include <Logger.h>
#include <ESP8266WiFi.h>
#include <PayloadHeader.h>

class ServerConnection
{
public:
    ServerConnection(Logger *logger);
    void begin(const char *ssid, const char *passphrase);
    void run();

private:
    bool tryReadPayload();

    void fatal(const char *message);
    void processPayload(unsigned char op_code, unsigned char *payload_buffer, size_t length);

    template <typename T>
    void sendPayload(unsigned char op_code, T *payload);

    template <typename T>
    bool map(unsigned char *payload_buffer, size_t length, T *&payload);

    Logger *_logger;
    WiFiClient _wifi_client;
    PayloadHeader _previous_header;
    bool _previous_header_processed;
};

#endif // SERVER_CONNECTION_h