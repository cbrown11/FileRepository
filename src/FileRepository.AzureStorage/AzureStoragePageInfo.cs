using Common.Models.Paging;

namespace FileRepository.AzureStorage
{
    public class AzureStoragePageInfo : PageInfo
    {
        public AzureStoragePageInfo(string? startContinuationToken, string endContinuationToken)

        {
            HasPreviousPage =  !string.IsNullOrEmpty(startContinuationToken);
            HasNextPage = endContinuationToken != string.Empty;
            StartCursor = startContinuationToken;
            EndCursor = endContinuationToken;
        }
    }  
}
