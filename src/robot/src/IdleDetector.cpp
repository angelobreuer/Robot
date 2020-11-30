#include <IdleDetector.h>
#include <Arduino.h>

IdleDetector::IdleDetector(unsigned long threshold)
    : _threshold(threshold), _idle_start(0), _violations(0), _violation_reset(0)
{
}

void IdleDetector::notifyStart()
{
    _idle_start = millis();
}

void IdleDetector::notifyEnd()
{
    if (millis() >= _violation_reset)
    {
        _violations = 0;
    }

    if (millis() - _idle_start >= _threshold)
    {
        _violation_reset = millis() + 5000;
        _violations++;
    }
}

bool IdleDetector::idle()
{
    return _violations < 10;
}