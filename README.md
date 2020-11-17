# Archive Processing Service
Archive Processing Service is a process to facilitate the rebuild functionality on files within an archive. 

Upon start-up it will read the archive file in the input location, unpack the archive, send adaptation requests for each of the files with in the archive, and finally rebuild the archive with the rebuilt files.

### Built With
- .NET Core
- Docker
