#ifndef PAYLOAD_HEADER_h
#define PAYLOAD_HEADER_h
#pragma once

#include <Arduino.h>

struct PayloadHeader
{
    int8_t op_code;
    uint16_t length;
};

#endif // PAYLOAD_HEADER_h