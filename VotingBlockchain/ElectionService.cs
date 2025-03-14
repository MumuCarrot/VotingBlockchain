using VotingBlockchain.Abstract;

namespace VotingBlockchain
{
    public class ElectionService : AService
    {
        public ElectionService() : base() 
        {
        }

        public override void Start() 
        {
            if (serviceTask == null || serviceTask.IsCompleted) 
            {
                isRunning = true;
                serviceTask = new Task(async () =>
                {
                    while (isRunning)
                    {
                        try
                        {
                            
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Mining error: {ex.Message}");
                        }

                        await Task.Delay(5000);
                    }
                });
                serviceTask.Start();
            }
        }

        public override void Stop()
        {
            isRunning = false;
            serviceTask?.Wait();
        }
    }
}
