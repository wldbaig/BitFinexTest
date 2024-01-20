using BitFinexP2P.Proto;
using Grpc.Core;
using Microsoft.Data.Sqlite;

namespace BitFinexP2PServer
{
    public class AuctionServer : AuctionService.AuctionServiceBase
    {
        private static List<IServerStreamWriter<BroadcastMessage>> connectedClients = new List<IServerStreamWriter<BroadcastMessage>>();
         
        private string connectionString = "Data Source=BitFinex.db";

        public AuctionServer()
        {
            // Create the SQLite database table if it does not exist
            InitializeDatabase();
        }

        /// <summary>
        /// Initializes a SQLite database by creating "Auctions" and "Bids" tables with specific column definitions.
        /// </summary>
        private void InitializeDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Create the Auctions table
                using (SqliteCommand command = new SqliteCommand(
                    "CREATE TABLE IF NOT EXISTS Auctions (AuctionId TEXT PRIMARY KEY, StartingPrice REAL, ItemName TEXT);", connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create the Bids table
                using (SqliteCommand command = new SqliteCommand(
                    "CREATE TABLE IF NOT EXISTS Bids (AuctionId TEXT, BidderId TEXT, BidAmount REAL);", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Initiates an auction by inserting or updating the "Auctions" table with the provided auction details. 
        /// Notifies connected clients about the auction creation.
        /// </summary>
        /// <param name="request">The auction request containing AuctionId, StartingPrice, and ItemName.</param>
        /// <param name="context">The server call context.</param>
        /// <returns>A Task containing a BoolResponse indicating the success of the operation.</returns>
        public override Task<BoolResponse> InitiateAuction(AuctionRequest request, ServerCallContext context)
        {
            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Insert or update the Auctions table
                    using (SqliteCommand command = new SqliteCommand(
                        "INSERT INTO Auctions (AuctionId, StartingPrice) VALUES (@AuctionId, @StartingPrice);", connection))
                    {
                        command.Parameters.AddWithValue("@AuctionId", request.AuctionId);
                        command.Parameters.AddWithValue("@StartingPrice", request.StartingPrice);
                        command.Parameters.AddWithValue("@ItemName", request.ItemName);
                        command.ExecuteNonQuery();
                    }

                }

                // Notify connected clients about the auction creation
                foreach (var client in connectedClients)
                {
                    try
                    {
                        client.WriteAsync(new BroadcastMessage { Message = $"New Auction is created for Item: {request.ItemName}" });
                    }
                    catch (Exception e)
                    {
                        // Handle exceptions or remove disconnected clients from the list
                    }
                }

                return Task.FromResult(new BoolResponse { Success = true });
            }
            catch (SqliteException ex)
            {
                // Handle SQLite-specific exceptions
                Console.WriteLine($"SQLite Error: {ex.Message}");
                return Task.FromResult(new BoolResponse { Success = false });
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Error initiating auction: {ex.Message}");
                return Task.FromResult(new BoolResponse { Success = false });
            }
        }

        /// <summary>
        /// Places a bid by checking if the auction exists and if the bid amount is higher than the starting price.
        /// Inserts the bid into the corresponding "Bids" table and notifies connected clients about the bid creation.
        /// </summary>
        /// <param name="request">The bid request containing AuctionId, BidderId, and BidAmount.</param>
        /// <param name="context">The server call context.</param>
        /// <returns>A Task containing a BoolResponse indicating the success of the bid placement.</returns>
        public override Task<BoolResponse> PlaceBid(BidRequest request, ServerCallContext context)
        {
            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Check if the auction exists and the bid amount is higher than the starting price
                    using (SqliteCommand checkCommand = new SqliteCommand(
                        "SELECT StartingPrice FROM Auctions WHERE AuctionId = @AuctionId;", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@AuctionId", request.AuctionId);
                        var startingPrice = Convert.ToDouble(checkCommand.ExecuteScalar());

                        if (startingPrice < request.BidAmount)
                        {
                            // Insert the bid into the corresponding Bids table
                            using (SqliteCommand insertCommand = new SqliteCommand(
                                $"INSERT INTO Bids (AuctionId, BidderId, BidAmount) VALUES (@AuctionId, @BidderId, @BidAmount);", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@BidderId", request.BidderId);
                                insertCommand.Parameters.AddWithValue("@BidAmount", request.BidAmount);
                                insertCommand.Parameters.AddWithValue("@AuctionId", request.AuctionId);
                                insertCommand.ExecuteNonQuery();
                            }

                            return Task.FromResult(new BoolResponse { Success = true });
                        }
                    }
                }

                // Notify connected clients about the Bids
                foreach (var client in connectedClients)
                {
                    try
                    {
                        client.WriteAsync(new BroadcastMessage { Message = $"New BID is created for AuctionID: {request.AuctionId} and BidderAmount: {request.BidAmount}" });
                    }
                    catch (Exception e)
                    {
                        // Handle exceptions or remove disconnected clients from the list
                    }
                }

                return Task.FromResult(new BoolResponse { Success = false });
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, return an error response)
                Console.WriteLine($"Error placing bid: {ex.Message}");
                return Task.FromResult(new BoolResponse { Success = false });
            }
        }

        /// <summary>
        /// Concludes an auction by retrieving the winning bid from the specific "Bids" table associated with the given auctionId.
        /// Notifies connected clients about the auction result.
        /// </summary>
        /// <param name="request">The auctionId as a StringValue.</param>
        /// <param name="context">The server call context.</param>
        /// <returns>A Task containing an AuctionResult indicating the winner and winning bid amount.</returns>
        public override Task<AuctionResult> ConcludeAuction(StringValue request, ServerCallContext context)
        {
            try
            {
                var auctionId = request.Value;

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Retrieve the winning bid from the specific Bids table associated with the auctionId
                    using (SqliteCommand command = new SqliteCommand(
                        $"SELECT BidderId, BidAmount FROM Bids WHERE AuctionId = @AuctionId ORDER BY BidAmount DESC LIMIT 1;", connection))
                    {
                        command.Parameters.AddWithValue("@AuctionId", auctionId);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var result = new AuctionResult
                                {
                                    AuctionId = auctionId,
                                    WinnerId = reader["BidderId"].ToString(),
                                    WinningBid = Convert.ToDouble(reader["BidAmount"])
                                };


                                // Notify connected clients about the Bids
                                foreach (var client in connectedClients)
                                {
                                    try
                                    {
                                        client.WriteAsync(new BroadcastMessage { Message = $"Auction AuctionID: {auctionId} is won by Winner: {reader["BidderId"].ToString()}" });
                                    }
                                    catch (Exception e)
                                    {
                                        // Handle exceptions or remove disconnected clients from the list
                                    }
                                }

                                return Task.FromResult(result);
                            }
                        }
                    }
                }

                return Task.FromResult(new AuctionResult());
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, return an error response)
                Console.WriteLine($"Error concluding auction: {ex.Message}");
                return Task.FromResult(new AuctionResult());
            }
        }
    } 
}
