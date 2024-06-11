using LargeFileSorter;
using LargeFileSorter.Services;

SortingService.PreSorting();
SortingService.Sort();
// ChatGPT solution. Should investigate
ExternalSort.Go();
//------------------------------------
SortingService.PostSorting();