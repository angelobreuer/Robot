#ifndef MOTOR_h
#define MOTOR_h

#include <Arduino.h>

class Motor
{
public:
    Motor(unsigned char enable_pin, unsigned char input1_pin, unsigned char input2_pin, unsigned char ct_pin, float analog_range = 1024);

    void forward(float speed);
    void reverse(float speed);
    void disable();
    void enable();
    void coasting();
    void braking();

    float current();

    unsigned short speedToAnalog(float speed);
    float analogToSpeed(unsigned short value);

private:
    unsigned char _enable_pin;
    unsigned char _input1_pin;
    unsigned char _input2_pin;
    unsigned char _ct_pin;
    float _analog_range;
};

#endif // !MOTOR_h