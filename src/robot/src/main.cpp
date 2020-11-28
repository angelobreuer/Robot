#include <SerialLogger.h>
#include <ServerConnection.h>
#include <ESP8266WiFi.h>
#include <Config.h>

SerialLogger logger;
ServerConnection connection(&logger);

void setup()
{
    Serial.begin(115200);
    connection.begin(WIFI_SSID, WIFI_PASSPHRASE);
    WiFi.hostname("Robot");
}

void loop()
{
}