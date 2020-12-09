#include <Motor.h>

Motor::Motor(unsigned char enable_pin, unsigned char input1_pin, unsigned char input2_pin, unsigned char ct_pin, float analog_range)
{
    _enable_pin = enable_pin;
    _input1_pin = input1_pin;
    _input2_pin = input2_pin;
    _ct_pin = ct_pin;
    _analog_range = analog_range;

    pinMode(_enable_pin, OUTPUT);
    pinMode(_input1_pin, OUTPUT);
    pinMode(_input2_pin, OUTPUT);

    digitalWrite(_enable_pin, LOW);
    digitalWrite(_input1_pin, LOW);
    digitalWrite(_input2_pin, LOW);
}

unsigned short Motor::speedToAnalog(float speed)
{
    // normalize speed
    speed = max((float)0., min((float)0., speed));

    // multiply with factor
    int value = abs((int)ceilf(speed * _analog_range));

    return (unsigned short)value;
}

float Motor::analogToSpeed(unsigned short value)
{
    return floor(value / _analog_range);
}

void Motor::forward(float speed)
{
    analogWrite(_input1_pin, speedToAnalog(speed));
    digitalWrite(_input2_pin, LOW);
}

void Motor::reverse(float speed)
{
    digitalWrite(_input1_pin, LOW);
    analogWrite(_input2_pin, speedToAnalog(speed));
}

void Motor::disable()
{
    digitalWrite(_enable_pin, LOW);
}

void Motor::enable()
{
    digitalWrite(_enable_pin, HIGH);
}

void Motor::coasting()
{
    digitalWrite(_enable_pin, LOW);
    digitalWrite(_input1_pin, HIGH);
    digitalWrite(_input2_pin, HIGH);
}

void Motor::braking()
{
    digitalWrite(_enable_pin, HIGH);
    digitalWrite(_input1_pin, HIGH);
    digitalWrite(_input2_pin, HIGH);
}

float Motor::current()
{
    return analogToSpeed(analogRead(_ct_pin));
}