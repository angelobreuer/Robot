#ifndef IDLE_DETECTOR_h
#define IDLE_DETECTOR_h
#pragma once

class IdleDetector
{
public:
    IdleDetector(unsigned long threshold = 50);
    void notifyStart();
    void notifyEnd();
    bool idle();

    unsigned long _threshold;
    unsigned long _idle_start;
    unsigned int _violations;
    unsigned long _violation_reset;
};

#endif // IDLE_DETECTOR_h