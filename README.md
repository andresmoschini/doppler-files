# Doppler-files

This service is going to be a common interface between Doppler and external storage providers. It exposes endpoints for managing user's files as a Linux filesystem. Initially one provider is going to be implemented but It could have multiple storage providers for file operations between them.

## How Can I access to the endpoints ?
All files and folders are going to be considerer private for writing. Only authenticated user can perform write operation over their folder. Unauthenticated users can access for read-only for a while getting the download URL of a file.

### Token authentication
You can get a user token from Doppler App after logged in:

```
GET https://appqa.fromdoppler.net/DopplerFiles/GetAuthorizationToken
GET https://appint.fromdoppler.net/DopplerFiles/GetAuthorizationToken
GET https://app2.fromdoppler.com/DopplerFiles/GetAuthorizationToken
```

Superusers have full access to all user's folders and files.

### Upload file endpoint
Users can access using their email accounts in the URL and a token in the request header. The request has PathFile of the file which is going to be relative to the user folder, override parameter to force overriding in the case of a file already exists, and the file content as an array of bytes.

```
POST https://apisint.fromdoppler.net/files/[user]@makingsense.com
{
"PathFile" : "/folder1/file2.pdf",
"Override" : true,
"Content" : "SEkdUlMdVlcA"
}

POST https://apisqa.fromdoppler.net/files/[user]@makingsense.com
{...}
POST https://apis.fromdoppler.com/files/[user]@makingsense.com
{...}
```

Superusers can access directly with a superuser token and a shorter URL.
```
POST https://apis.fromdoppler.com/files
{...}
```

### Donwload Link endpoint
This endpoint does not require an authorization token. It is public.

```
GET https://apisint.fromdoppler.net/files/%2FUsers%2F[user]%40makingsense.com%2Ffile1.pdf

GET https://apisqa.fromdoppler.net/files/%2FUsers%2F[user]%40makingsense.com%2Ffile1.pdf

GET https://apis.fromdoppler.com/files/%2FUsers%2F[user]%40makingsense.com%2Ffile1.pdf
```
