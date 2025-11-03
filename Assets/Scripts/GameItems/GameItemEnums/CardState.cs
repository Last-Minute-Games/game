public enum CardState
{
    Idle,          // resting in the hand
    Hovered,       // mouse over
    Dragging,      // selected & following cursor
    Targeting,     // selected & showing arrow toward a target while card is static on bottom center
    Played,        // successfully used
}
