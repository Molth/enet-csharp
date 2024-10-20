using size_t = nint;
using unsafe ENetListIterator = enet.ENetListNode*;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static void enet_list_clear(ENetList* list)
        {
            list->sentinel.next = &list->sentinel;
            list->sentinel.previous = &list->sentinel;
        }

        public static ENetListIterator enet_list_insert(ENetListIterator position, void* data)
        {
            ENetListIterator result = (ENetListIterator)data;

            result->previous = position->previous;
            result->next = position;

            result->previous->next = result;
            position->previous = result;

            return result;
        }

        public static void* enet_list_remove(ENetListIterator position)
        {
            position->previous->next = position->next;
            position->next->previous = position->previous;

            return position;
        }

        public static ENetListIterator enet_list_move(ENetListIterator position, void* dataFirst, void* dataLast)
        {
            ENetListIterator first = (ENetListIterator)dataFirst,
                last = (ENetListIterator)dataLast;

            first->previous->next = last->next;
            last->next->previous = first->previous;

            first->previous = position->previous;
            last->next = position;

            first->previous->next = first;
            position->previous = last;

            return first;
        }

        public static size_t enet_list_size(ENetList* list)
        {
            size_t size = 0;
            ENetListIterator position;

            for (position = enet_list_begin(list);
                 position != enet_list_end(list);
                 position = enet_list_next(position))
                ++size;

            return size;
        }
    }
}