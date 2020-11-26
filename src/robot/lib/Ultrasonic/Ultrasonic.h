#include "Arduino.h"

/**
 * A class used to manage ultrasonic sensors (HC-SR04 like).
 */
class UltrasonicSensor
{
public:
    UltrasonicSensor(uint8_t trigger, uint8_t echo);

    /**
     * Measures the time in microseconds required to receive a signal back.
     * @return the time in microseconds needed. 
     */
    uint64_t pulseTime();

    /**
     * Measures the distance in centimeters travelled before an object was hit.
     * @return the distance in centimeters travelled before an object was hit 
     */
    float pulse();

private:
    uint8_t _trigger;
    uint8_t _echo;
};