using BitFinexP2P.Proto;
using Grpc.Core;

class Program
{
    const int Port = 50051;
    public static void Main(string[] args)
    {    
        // Create a communication channel with the Auction server
        Channel channel = new Channel("localhost", Port, ChannelCredentials.Insecure);
        var client = new AuctionService.AuctionServiceClient(channel);

        // Flag to track whether the user has requested to exit the application
        bool exitRequested = false;
        do
        {
            // Display menu options for user interaction
            Console.WriteLine("Choose an action:");
            Console.WriteLine("1. Initialize Auction");
            Console.WriteLine("2. Place Bid");
            Console.WriteLine("3. Conclude Auction");
            Console.WriteLine("4. Exit");

            // Get the user's choice
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine(); // To move to the next line

            switch (choice)
            {
                case '1':
                    InitiateAuction(client);
                    break;
                case '2':
                    PlaceBid(client);
                    break;
                case '3':
                    ConcludeAuction(client);
                    break;
                case '4':
                    exitRequested = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Try again...");
                    break;
            }

        } while (!exitRequested);

        // Shutdown the communication channel
        channel.ShutdownAsync().Wait();

        // Display a message and wait for a key press before exiting
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Initiates an auction by prompting the user to enter auction information, including Auction ID, Item Name, and Starting Price.
    /// Sends the provided auction details to the Auction server using the given client and displays the success status of the operation.
    /// </summary>
    /// <param name="client">The Auction service client used for communication with the server.</param>
    private static void InitiateAuction(AuctionService.AuctionServiceClient client)
    {
        Console.WriteLine("Enter auction information:");
        Console.Write("Auction ID: ");
        string auctionId = Console.ReadLine();

        Console.Write("Item Name: ");
        string itemName = Console.ReadLine();

        Console.Write("Starting Price: ");
        double startingPrice;
        while (!double.TryParse(Console.ReadLine(), out startingPrice))
        {
            Console.WriteLine("Invalid input. Please enter a valid number for Starting Price:");
        }

        var response = client.InitiateAuction(new AuctionRequest
        {
            AuctionId = auctionId,
            ItemName = itemName,
            StartingPrice = startingPrice
        });

        Console.WriteLine($"Auction initiated: {response.Success}");
    }

    /// <summary>
    /// Places a bid by prompting the user to enter bid information, including Auction ID, Bidder ID, and Bid Amount.
    /// Sends the provided bid details to the Auction server using the given client and displays the success status of the operation.
    /// </summary>
    /// <param name="client">The Auction service client used for communication with the server.</param>
    private static void PlaceBid(AuctionService.AuctionServiceClient client)
    {
        Console.WriteLine("Enter bid information:");
        Console.Write("Auction ID: ");
        string auctionId = Console.ReadLine();

        Console.Write("Bidder ID: ");
        string bidderId = Console.ReadLine();

        Console.Write("Bid Amount: ");
        double bidAmount;
        while (!double.TryParse(Console.ReadLine(), out bidAmount))
        {
            Console.WriteLine("Invalid input. Please enter a valid number for Bid Amount:");
        }

        var response = client.PlaceBid(new BidRequest
        {
            AuctionId = auctionId,
            BidderId = bidderId,
            BidAmount = bidAmount
        });

        Console.WriteLine($"Bid placed: {response.Success}");
    }

    /// <summary>
    /// Concludes an auction by prompting the user to enter the Auction ID.
    /// Sends the provided Auction ID to the Auction server using the given client and displays the auction result, including the winner and winning bid amount.
    /// </summary>
    /// <param name="client">The Auction service client used for communication with the server.</param>
    private static void ConcludeAuction(AuctionService.AuctionServiceClient client)
    {
        Console.Write("Enter the Auction ID to conclude: ");
        string auctionId = Console.ReadLine();

        var response = client.ConcludeAuction(new StringValue
        {
            Value = auctionId
        });
         
        Console.WriteLine($"Auction concluded. Winner: {response.WinnerId} with winning bid {response.WinningBid}"); 
    }
}