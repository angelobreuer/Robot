#ifndef CIRCULAR_BUFFER_h
#define CIRCULAR_BUFFER_h
#pragma once

#include <Arduino.h>

class CircularBuffer
{
public:
    CircularBuffer(size_t capacity);
    ~CircularBuffer();

    size_t size() const { return _size; }
    size_t capacity() const { return _capacity; }
    size_t write(const char *data, size_t bytes);
    size_t read(char *data, size_t bytes);

private:
    size_t _head;
    size_t _tail;
    size_t _size;
    size_t _capacity;
    char *_buffer;
};

#endif // CIRCULAR_BUFFER_h