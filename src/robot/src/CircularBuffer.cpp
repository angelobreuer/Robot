#include <CircularBuffer.h>

CircularBuffer::CircularBuffer(size_t capacity)
    : _head(0), _tail(0), _size(0), _capacity(capacity)
{
    _buffer = new char[capacity];
}

CircularBuffer::~CircularBuffer()
{
    delete[] _buffer;
}

size_t CircularBuffer::write(const char *data, size_t bytes)
{
    if (bytes == 0)
    {
        return 0;
    }

    size_t capacity = _capacity;
    size_t bytes_to_write = std::min(bytes, capacity - _size);

    // Write in a single step
    if (bytes_to_write <= capacity - _tail)
    {
        memcpy(_buffer + _tail, data, bytes_to_write);
        _tail += bytes_to_write;
        if (_tail == capacity)
            _tail = 0;
    }
    // Write in two steps
    else
    {
        size_t size_1 = capacity - _tail;
        memcpy(_buffer + _tail, data, size_1);
        size_t size_2 = bytes_to_write - size_1;
        memcpy(_buffer, data + size_1, size_2);
        _tail = size_2;
    }

    _size += bytes_to_write;
    return bytes_to_write;
}

size_t CircularBuffer::read(char *data, size_t bytes)
{
    if (bytes == 0)
    {
        return 0;
    }

    size_t capacity = _capacity;
    size_t bytes_to_read = std::min(bytes, _size);

    // Read in a single step
    if (bytes_to_read <= capacity - _head)
    {
        memcpy(data, _buffer + _head, bytes_to_read);
        _head += bytes_to_read;
        if (_head == capacity)
            _head = 0;
    }
    // Read in two steps
    else
    {
        size_t size_1 = capacity - _head;
        memcpy(data, _buffer + _head, size_1);
        size_t size_2 = bytes_to_read - size_1;
        memcpy(data + size_1, _buffer, size_2);
        _head = size_2;
    }

    _size -= bytes_to_read;
    return bytes_to_read;
}