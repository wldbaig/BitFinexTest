using BitFinexP2P.Proto;
using BitFinexP2PServer;
using Grpc.Core;

class Program
{
    const int Port = 50051;

    public static void Main(string[] args)
    {
        // Create an instance of the Auction server
        var server = new Server
        {
            Services = { AuctionService.BindService(new AuctionServer()) }, // Bind the Auction service to the server
            Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) } // Set up server port and credentials
        };
        // Start the server
        server.Start();

        // Display server information
        Console.WriteLine("Auction server listening on port " + Port);
        Console.WriteLine("Press any key to stop the server...");

        // Wait for a key press to stop the server
        Console.ReadKey();

        // Shutdown the server when the key is pressed
        server.ShutdownAsync().Wait();
    }
}