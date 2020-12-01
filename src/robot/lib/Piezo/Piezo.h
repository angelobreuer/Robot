#ifndef PIEZO_h
#define PIEZO_h
#pragma once

#include <Arduino.h>

class Piezo
{
public:
    Piezo(uint8_t pin);
    void tone(uint32_t frequency, uint64_t duration);

private:
    uint8_t _pin;
};

#endif // PIEZO_h