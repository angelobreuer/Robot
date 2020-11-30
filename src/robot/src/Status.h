#ifndef STATUS_h
#define STATUS_h
#pragma once

enum Status : unsigned char
{
    /**
     * Denotes that the client is currently initializing.
     */
    INIT,

    /**
     * Denotes that the client is in idle.
     */
    IDLE,

    /**
     * Denotes that the client is under load.
     */
    LOAD,
};

#endif // STATUS_h