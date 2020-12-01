#include <Piezo.h>

Piezo::Piezo(uint8_t pin) : _pin(pin)
{
    pinMode(pin, OUTPUT);
}

void Piezo::tone(uint32_t frequency, uint64_t duration)
{
    ::tone(_pin, frequency, duration);
}