#include <main.h>

SerialLogger logger;
ServerConnection connection(&logger);
IdleDetector idle_detector;
Status previous_status;

void setup()
{
    previous_status = Status::INIT;

    Serial.begin(115200);
    connection.begin(WIFI_SSID, WIFI_PASSPHRASE, HOST_ADDRESS);
    WiFi.hostname("Robot");
}

void loop()
{
    idle_detector.notifyStart();
    tick();
    idle_detector.notifyEnd();
}

void tick()
{
    connection.run();

    Status new_status = idle_detector.idle() ? Status::IDLE : Status::LOAD;

    if (previous_status != new_status)
    {
        logger.writef("Status updated: %d.", new_status);
        connection.updateStatus(new_status);
        previous_status = new_status;
    }
}