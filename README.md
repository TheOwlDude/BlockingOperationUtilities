# BlockingOperationUtilities

This library is a response to some issues encountered using failover connections to ActiveMQ. Failover connections
are in some ways marvelous but a side effect of using them is that when the underlying TCP connection to the ActiveMQ
server is lost every call to the client library blocks.

The library supplies two options for producers.

Fire and Forget

This option is implemented by the TransientItemWorkUtility class. It is appropriate for items that it is OK if they are not 
processed. In particular this strategy is useful for repetitively produced items like heartbeats or status updates that lose 
value if they do not reach their consumers in a timely manner. This implementation uses a background thread that may block 
indefinitely, but producers are not waiting nor especially concerned if the work is ever performed.

Timeout and Retry

The idea here is to allow callers to provide an object to indicate the desire to cancel. The underlying operation will timeout 
after a brief interval but as long as the caller has not indicated desire to cancel, the operation will be retried until 
successful. This provides pseudo-blocking behavior to clients but with the ability to break out. This is especially useful when 
shutting down. This strategy is not yet implemented.
    
 
