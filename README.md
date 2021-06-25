# Heimdall API clients

## Introduction

This repository provides three sample client implementations in different languages that calls the Heimdall Cloud API as a service daemon application. Follow the read me in each client to get started.

Client types
* .Net 5 console application
* Java application
* Python script

Each client authenticates with the OAuth 2.0 client credentials flow using certicates
* First the client obtains an access token
* Each token permits the client to consume the Heimdall API for one hour
* The client calls the Heimdall API with the token to retrieve the power line and line spans connected to this application
* The client calls the Heimdall API again to get aggregated current measurements for a line for the last 7 days

## Swagger

The swagger interface found [here](https://api.heimdallcloud.com/index.html) provides detailed descriptions about the different REST endpoints. 

It gives information about
* how to construct requests
* what response code and data to expect back
* the `try me` feature lets you experiment quickly with endpoints 
	* Click `authorize` and log in to the same account you use to login to the [cloud app](https://heimdallcloud.com/)

## Updates

**25.6.21** 
Added logic to use the `/api/beta/lines` and `/api/beta/aggregated-measurements` endpoints