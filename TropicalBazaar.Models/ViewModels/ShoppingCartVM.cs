namespace TropicalBazaar.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> AllProductsCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
