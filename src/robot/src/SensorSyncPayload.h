#ifndef SENSOR_SYNC_PAYLOAD_h
#define SENSOR_SYNC_PAYLOAD_h
#pragma once

#include <Arduino.h>

struct SensorSyncPayload
{
    uint8_t id;
    uint8_t value;
};

#endif // SENSOR_SYNC_PAYLOAD_h