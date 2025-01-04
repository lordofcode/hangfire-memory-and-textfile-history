# hangfire-memory-and-textfile-history
Hangfire offers a Memory only solution. But it has no history. This repository does.

🚧 DO NOT USE THIS CODE IN PRODUCTION!

Currently (2025-01-04) I put the code in this repository. It "works on my machine". But beware, some functions have not been implemented, there might be memory-issues.

Also cleaning jobs is not implemented. This might after a few days result in large history.txt files and memory-usage. 

And I saw that when you schedule a job with parameters, it does not work.

That said, if you want to use this code, feel free, go ahead. 

Probably I am going to use it in production and with the issues I will be confronted with, I will update this code in the near future.



A big shout-out to the creators of Hangfire, I love this product. You can find the source-code on https://github.com/HangfireIO/Hangfire