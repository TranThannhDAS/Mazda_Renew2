namespace backend.Helper
{
    public class Pagination<T>
    {
        public Pagination(int pageIndex, int pageSize, IReadOnlyList<T> data, int count, int totalPageIndex)
        {
            //Số trang
            PageIndex = pageIndex;
            //Số phần tử trong 1 page
            PageSize = pageSize;
            //Dữ Liệu Hiển thị trong 1 Page
            Data = data;
            //Đếm Tổng Phần Tử
            Count = count;
            TotalPageIndex = totalPageIndex;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IReadOnlyList<T>? Data { get;}
        public int Count { get; set; }
        public int TotalPageIndex { get; set; }
    }
}
