Service takes data from the Redis list (queue) and stores it in the blocked queue for further processing. 
The incoming model will be extended with new fields. 
Extended data sent to the Service Bus topic in butches. 
Size of butch - 5.
If butch is not completed and there is no data in the internal queue for 2 seconds sends an uncompleted batch
