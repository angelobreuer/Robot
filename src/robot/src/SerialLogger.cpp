#include <SerialLogger.h>

void SerialLogger::write(const char *buffer, size_t bytes)
{
    char temp[bytes + 1];
    memcpy(temp, buffer, bytes);
    temp[bytes] = '\n';

    Serial.write(temp, bytes + 1);
}

void SerialLogger::write(const char *buffer)
{
    write(buffer, strlen(buffer));
}

void SerialLogger::writef(const char *format, ...)
{
    va_list arg;
    va_start(arg, format);
    char temp[64];
    char *buffer = temp;
    size_t len = vsnprintf(temp, sizeof(temp) - 1, format, arg);
    va_end(arg);

    if (len > sizeof(temp) - 1)
    {
        buffer = new char[len + 2];
        va_start(arg, format);
        vsnprintf(buffer, len + 1, format, arg);
        va_end(arg);
    }

    buffer[len] = '\n';

    len = Serial.write((const uint8_t *)buffer, len + 1);

    if (buffer != temp)
    {
        delete[] buffer;
    }
}

void SerialLogger::flush()
{
    Serial.flush();
}