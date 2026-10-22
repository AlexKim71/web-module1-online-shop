INSERT INTO Categories (Id, Name) VALUES
(1, 'Електроніка'), (2, 'Дім і сад'), (3, 'Спорт');

INSERT INTO Resources (Id, CategoryId, Code, Name, PriceMinor, Capacity, Available, Sku, Brand) VALUES
(1, 1, 'PHONE-001', 'Смартфон Alpha', 199900, 10, 10, 'SKU-PHONE-001', 'Alpha'),
(2, 1, 'PHONE-002', 'Смартфон Beta', 199900, 5, 5, 'SKU-PHONE-002', 'Beta'),
(3, 1, 'LAPTOP-01', 'Ноутбук Gamma', 459900, 4, 4, 'SKU-LAPTOP-01', 'Gamma'),
(4, 2, 'LAMP-001', 'Настільна лампа', 89900, 12, 12, 'SKU-LAMP-001', 'LightHome'),
(5, 2, 'CHAIR-01', 'Стілець Office', 129900, 8, 8, 'SKU-CHAIR-01', 'HomeLine'),
(6, 2, 'TABLE-01', 'Стіл дубовий', 349900, 3, 3, 'SKU-TABLE-01', 'WoodLab'),
(7, 3, 'BALL-001', 'Футбольний м’яч', 59900, 20, 20, 'SKU-BALL-001', 'SportMax'),
(8, 3, 'MAT-001', 'Килимок для йоги', 45900, 15, 15, 'SKU-MAT-001', 'FlexFit'),
(9, 3, 'DUMB-001', 'Гантель 5 кг', 79900, 6, 6, 'SKU-DUMB-001', 'IronWay'),
(10, 1, 'TABLET-01', 'Планшет Delta', 279900, 7, 7, 'SKU-TABLET-01', 'Delta'),
(11, 2, 'VASE-001', 'Ваза керамічна', 34900, 9, 9, 'SKU-VASE-001', 'Ceramica'),
(12, 3, 'BIKE-001', 'Велосипед міський', 699900, 0, 0, 'SKU-BIKE-001', 'CityRide');