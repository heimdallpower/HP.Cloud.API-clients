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
* The client calls the Heimdall API with the token to retrieve the power line measurements for the latest 7 days

## Swagger

The swagger interface found [here](https://hp-cloud-api-app-dev.azurewebsites.net/index.html) provides detailed descriptions about the different REST endpoints. 

It gives information about
* how to construct requests
* what response code and data to expect back
* the `try me` feature lets you experiment quickly with endpoints 
	* Click `authorize` and log in to the same account you use to login to the [cloud app](https://heimdallcloud.com/)

## Updates

Updates to API will be described here.