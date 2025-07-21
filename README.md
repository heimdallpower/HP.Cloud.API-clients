# ⚠️ DEPRECATED

This repository is no longer actively maintained.

The code will continue to function, but we recommend using [Heimdall Power API SDK](https://github.com/heimdallpower/api-sdk) if possible.

No further updates or support are planned.

# Heimdall API clients

## Introduction

This repository provides three sample client implementations in different languages that calls the Heimdall Cloud API as a service daemon application. Follow the `README` in each client to get started.

Client types
* .Net 7 console application (uses the `HeimdallPower.CloudApi.Client` [nuget package](https://www.nuget.org/packages/HeimdallPower.CloudApi.Client/))
* Java application
* Python script

Each client authenticates with the `OAuth 2.0 client credentials flow` using client secrets
* First the client obtains an `access token`
* Each token permits the client to consume the Heimdall API for one hour
* Secondly the client calls the Heimdall API to get the connected to this application
* Lastly the client calls the Heimdall API again to get `aggregated current measurements` and `dynamic line ratings`  for a line
## Swagger

The Swagger interface found [here](https://api.heimdallcloud.com/index.html) provides detailed descriptions about the different REST endpoints. 

It gives information about
* how to construct requests
* what response code and data to expect back
* the `try me` feature lets you experiment quickly with endpoints 
	* Click `authorize` and log in to the same account you use to login to the [cloud app](https://heimdallcloud.com/)

## Updates
**02.02.23**
* Updated API clients to use Azure Active Directory B2C authentication
* NB! This authentication framework does not support certificates, and requires client application to authenticate using client secrets.

**29.08.22** 
* Restructured python client

**8.11.21** 
* Use HeimdallPower.CloudApi.Client nuget package in .NET client

**27.10.21** 
* Support usage of the developer API for testing purposes
* Output formatting improvements

**1.10.21** 
* Use the new V1 version of the Heimdall Cloud API

**15.9.21** 
* Use the new scope of the Heimdall Cloud API

**3.9.21** 
* JavaClient - improve structure for readability
* JavaClient - retrieve power line data for all lines connected to caller's identity

**13.8.21** 
* DotNetClient - improve structure for readability
* DotNetClient - retrieve power line data for all lines connected to caller's identity
* Updated clients to use correct dynamic line rating endpoint

**2.7.21** 
Implemented new endpoint to get dynamic line ratings at `/api/beta/dlr/aggregated-dlr`

**25.6.21** 
Added logic to use the `/api/beta/lines` and `/api/beta/aggregated-measurements` endpoints
