#ifndef LOGGER_h
#define LOGGER_h
#pragma once

#include <Arduino.h>

class Logger
{
public:
    virtual void write(const char *buffer) = 0;
    virtual void write(const char *buffer, size_t bytes) = 0;
    virtual void writef(const char *format, ...) = 0;
    virtual void flush() = 0;
};

#endif // LOGGER_h