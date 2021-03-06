#include "Event.H.txt"

Event::Event(const EventType type, const double time, const int node, const Message& message)
  : _type(type),_time(time),_node(node),_message(message)
{ 
}

EventType Event::getType() const 
{ 
  return _type; 
}
    
double Event::getTime() const 
{ 
  return _time; 
}

int Event::getNode() const 
{ 
  return _node; 
}

Message Event::getMessage() const
{
  return _message;
}
  
bool Event::operator<(const Event& rhs) const 
{
  return this->_time<rhs._time;
}
