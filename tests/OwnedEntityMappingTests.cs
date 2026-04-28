using Simple.AutoMapper.Core;

namespace Simple.AutoMapper.Tests
{
    // EF Core의 [Owned] 어트리뷰트를 시뮬레이션하는 커스텀 속성
    // 실제 EF Core의 OwnedAttribute와 동일하게 클래스에 붙이는 마커 어트리뷰트
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OwnedAttribute : Attribute { }

    /// <summary>
    /// [Owned] 어트리뷰트가 붙은 클래스를 포함하는 parent class의 매핑 검증
    /// </summary>
    [Collection("Mapper Tests")]
    public class OwnedEntityMappingTests
    {
        #region 시나리오 1: [Owned] 타입 프로퍼티를 가진 Parent class 매핑

        [Owned]
        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }

        public class Order
        {
            public Guid Id { get; set; }
            public string CustomerName { get; set; }
            public Address ShippingAddress { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }

        public class OrderDto
        {
            public Guid Id { get; set; }
            public string CustomerName { get; set; }
            public AddressDto ShippingAddress { get; set; }
        }

        [Fact]
        public void Map_ParentWithOwnedProperty_ShouldMapAllProperties()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<Address, AddressDto>();
            engine.CreateMap<Order, OrderDto>();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "홍길동",
                ShippingAddress = new Address
                {
                    Street = "강남대로 123",
                    City = "서울",
                    ZipCode = "06000"
                }
            };

            var dto = engine.MapInstance<Order, OrderDto>(order);

            Assert.NotNull(dto);
            Assert.Equal(order.Id, dto.Id);
            Assert.Equal("홍길동", dto.CustomerName);
            Assert.NotNull(dto.ShippingAddress);
            Assert.Equal("강남대로 123", dto.ShippingAddress.Street);
            Assert.Equal("서울", dto.ShippingAddress.City);
            Assert.Equal("06000", dto.ShippingAddress.ZipCode);
        }

        [Fact]
        public void Map_ParentWithNullOwnedProperty_ShouldMapNullCorrectly()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<Address, AddressDto>();
            engine.CreateMap<Order, OrderDto>();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "홍길동",
                ShippingAddress = null
            };

            var dto = engine.MapInstance<Order, OrderDto>(order);

            Assert.NotNull(dto);
            Assert.Equal(order.Id, dto.Id);
            Assert.Null(dto.ShippingAddress);
        }

        #endregion

        #region 시나리오 2: [Owned] base class를 상속받는 Parent class 매핑

        [Owned]
        public class OwnedBase
        {
            public string CreatedBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class Product : OwnedBase
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class ProductDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        [Fact]
        public void Map_ClassInheritingOwnedBase_ShouldMapInheritedProperties()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<Product, ProductDto>();

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "테스트 상품",
                Price = 9900m,
                CreatedBy = "admin",
                CreatedAt = now
            };

            var dto = engine.MapInstance<Product, ProductDto>(product);

            Assert.NotNull(dto);
            Assert.Equal(product.Id, dto.Id);
            Assert.Equal("테스트 상품", dto.Name);
            Assert.Equal(9900m, dto.Price);
            // [Owned] base class의 상속된 프로퍼티도 매핑되어야 함
            Assert.Equal("admin", dto.CreatedBy);
            Assert.Equal(now, dto.CreatedAt);
        }

        #endregion

        #region 시나리오 3: [Owned] 타입 안에 또 다른 [Owned] 타입 (중첩)

        [Owned]
        public class GeoLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        [Owned]
        public class StoreAddress
        {
            public string Street { get; set; }
            public GeoLocation Location { get; set; }
        }

        public class Store
        {
            public Guid Id { get; set; }
            public string StoreName { get; set; }
            public StoreAddress Address { get; set; }
        }

        public class GeoLocationDto
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class StoreAddressDto
        {
            public string Street { get; set; }
            public GeoLocationDto Location { get; set; }
        }

        public class StoreDto
        {
            public Guid Id { get; set; }
            public string StoreName { get; set; }
            public StoreAddressDto Address { get; set; }
        }

        [Fact]
        public void Map_NestedOwnedTypes_ShouldMapAllLevels()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<GeoLocation, GeoLocationDto>();
            engine.CreateMap<StoreAddress, StoreAddressDto>();
            engine.CreateMap<Store, StoreDto>();

            var store = new Store
            {
                Id = Guid.NewGuid(),
                StoreName = "강남점",
                Address = new StoreAddress
                {
                    Street = "테헤란로 456",
                    Location = new GeoLocation
                    {
                        Latitude = 37.5012,
                        Longitude = 127.0396
                    }
                }
            };

            var dto = engine.MapInstance<Store, StoreDto>(store);

            Assert.NotNull(dto);
            Assert.Equal(store.Id, dto.Id);
            Assert.Equal("강남점", dto.StoreName);
            Assert.NotNull(dto.Address);
            Assert.Equal("테헤란로 456", dto.Address.Street);
            Assert.NotNull(dto.Address.Location);
            Assert.Equal(37.5012, dto.Address.Location.Latitude);
            Assert.Equal(127.0396, dto.Address.Location.Longitude);
        }

        #endregion

        #region 시나리오 4: [Owned] 타입의 컬렉션을 가진 Parent class

        [Owned]
        public class OrderLine
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public class Invoice
        {
            public Guid Id { get; set; }
            public string InvoiceNumber { get; set; }
            public List<OrderLine> Lines { get; set; }
        }

        public class OrderLineDto
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        public class InvoiceDto
        {
            public Guid Id { get; set; }
            public string InvoiceNumber { get; set; }
            public List<OrderLineDto> Lines { get; set; }
        }

        [Fact]
        public void Map_ParentWithOwnedCollection_ShouldMapCollectionItems()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<OrderLine, OrderLineDto>();
            engine.CreateMap<Invoice, InvoiceDto>();

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = "INV-2026-001",
                Lines = new List<OrderLine>
                {
                    new OrderLine { ProductName = "상품A", Quantity = 2, UnitPrice = 5000m },
                    new OrderLine { ProductName = "상품B", Quantity = 1, UnitPrice = 12000m }
                }
            };

            var dto = engine.MapInstance<Invoice, InvoiceDto>(invoice);

            Assert.NotNull(dto);
            Assert.Equal(invoice.Id, dto.Id);
            Assert.Equal("INV-2026-001", dto.InvoiceNumber);
            Assert.NotNull(dto.Lines);
            Assert.Equal(2, dto.Lines.Count);
            Assert.Equal("상품A", dto.Lines[0].ProductName);
            Assert.Equal(2, dto.Lines[0].Quantity);
            Assert.Equal(5000m, dto.Lines[0].UnitPrice);
            Assert.Equal("상품B", dto.Lines[1].ProductName);
        }

        #endregion

        #region 시나리오 5: [Owned] 어트리뷰트가 매핑 동작에 영향 없음을 직접 비교

        public class PlainAddress
        {
            public string Street { get; set; }
            public string City { get; set; }
        }

        [Owned]
        public class OwnedAddress
        {
            public string Street { get; set; }
            public string City { get; set; }
        }

        public class OrderWithPlain
        {
            public Guid Id { get; set; }
            public PlainAddress Address { get; set; }
        }

        public class OrderWithOwned
        {
            public Guid Id { get; set; }
            public OwnedAddress Address { get; set; }
        }

        public class PlainAddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
        }

        public class OwnedAddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
        }

        public class OrderWithPlainDto
        {
            public Guid Id { get; set; }
            public PlainAddressDto Address { get; set; }
        }

        public class OrderWithOwnedDto
        {
            public Guid Id { get; set; }
            public OwnedAddressDto Address { get; set; }
        }

        [Fact]
        public void Map_OwnedAttribute_ShouldNotAffectMappingBehavior()
        {
            var engine = Mapper.Reset();
            engine.CreateMap<PlainAddress, PlainAddressDto>();
            engine.CreateMap<OrderWithPlain, OrderWithPlainDto>();
            engine.CreateMap<OwnedAddress, OwnedAddressDto>();
            engine.CreateMap<OrderWithOwned, OrderWithOwnedDto>();

            var id = Guid.NewGuid();

            var orderWithPlain = new OrderWithPlain
            {
                Id = id,
                Address = new PlainAddress { Street = "테스트로 1", City = "서울" }
            };

            var orderWithOwned = new OrderWithOwned
            {
                Id = id,
                Address = new OwnedAddress { Street = "테스트로 1", City = "서울" }
            };

            var plainDto = engine.MapInstance<OrderWithPlain, OrderWithPlainDto>(orderWithPlain);
            var ownedDto = engine.MapInstance<OrderWithOwned, OrderWithOwnedDto>(orderWithOwned);

            // [Owned] 유무와 관계없이 동일하게 매핑되어야 함
            Assert.Equal(plainDto.Id, ownedDto.Id);
            Assert.Equal(plainDto.Address.Street, ownedDto.Address.Street);
            Assert.Equal(plainDto.Address.City, ownedDto.Address.City);
        }

        #endregion
    }
}
