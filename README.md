# Bitfinex P2P Auction System - C#


## Introduction:

This project demonstrates a simple Peer-to-Peer auction system using Remote Procedure Calls (RPC) in C#. The system allows users to initiate auctions, place bids, and handles auction closures with distributed transaction handling.

## Project Structure:
#### BitFinexServer

- The server project handles auction initialization, bid handling, and auction conclusion.
- Run the server project once to initiate the auction system.
- Handles RPC communication with clients.

#### BitFinexClient

- The client project allows users to initiate auctions, place bids, and receive notifications.
- Run multiple instances of the client project to simulate different participants in the auction.

## Setup Instructions:

#### Server Setup:

- Open the `BitFinexP2PServer` project in Visual Studio and Set is as Startup Project.
- Build and run the project to start the server.
- The server will handle RPC communication with clients.

#### Client Setup:

- Open the `BitFinexP2PClient` project in Visual Studio and Set is as Startup Project.
- Build and run multiple instances of the client project to simulate different participants.
- Follow the console prompts to initiate auctions, place bids, and receive notifications.

## Important Notes:
- Ensure the server is running before starting any client instances.

## Dependencies:
- Ensure you have the necessary dependencies installed (SQLite, gRPC).

## Troubleshooting:
- If you encounter issues, check console outputs for error messages.
Ensure the correct project is set as the startup project.

