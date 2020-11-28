#ifndef SERIAL_LOGGER_h
#define SERIAL_LOGGER_h
#pragma once

#include <Logger.h>

class SerialLogger : public Logger
{
public:
    void write(const char *buffer);
    void write(const char *buffer, size_t bytes);
    void writef(const char *format, ...);
    void flush();
};

#endif // SERIAL_LOGGER_h