#include "Ultrasonic.h"

UltrasonicSensor::UltrasonicSensor(uint8_t trigger, uint8_t echo)
    : _trigger(trigger), _echo(echo)
{
    pinMode(trigger, OUTPUT);
    pinMode(echo, INPUT);

    digitalWrite(_trigger, LOW);
}

uint64_t UltrasonicSensor::pulseTime()
{
    digitalWrite(_trigger, HIGH);
    delay(10);
    digitalWrite(_trigger, LOW);

    return pulseIn(_echo, HIGH);
}

float UltrasonicSensor::pulse()
{
    return pulseTime() / (2 * 29.1F);
}
