SELECT
	piv.UserName,
	(
		SELECT COUNT(*)
		FROM Orders o
			JOIN Shoppers s ON s.ShopperID = o.ShopperID
		WHERE s.UserName = piv.UserName
	) AS Total,
	ISNULL([KitKat], 0) AS [KitKat],
	ISNULL([5th Avenue], 0) AS [5th Avenue],
	ISNULL([Butterfinger], 0) AS [Butterfinger],
	ISNULL([Crunch], 0) AS [Crunch],
	ISNULL([Ghirardelli Intense Dark], 0) AS [Ghirardelli Intense Dark],
	ISNULL([Godiva Chocolate and Almond], 0) AS [Godiva Chocolate and Almond],
	ISNULL([Hershey's], 0) AS [Hershey's],
	ISNULL([Hershey's Cookies & Cream], 0) AS [Hershey's Cookies & Cream],
	ISNULL([Lindt Extra Creamy], 0) AS [Lindt Extra Creamy],
	ISNULL([M&M's], 0) AS [M&M's],
	ISNULL([Peanut M&M's], 0) AS [Peanut M&M's],
	ISNULL([Snickers], 0) AS [Snickers],
	ISNULL([Twix], 0) AS [Twix]
FROM (
	SELECT
		s.UserName,
		p.Name AS ProductName,
		COUNT(*) AS Count
	FROM Shoppers s
		JOIN Orders o ON o.ShopperID = s.ShopperID
		JOIN Products p ON p.ProductID = o.ProductID
	GROUP BY s.UserName, p.Name
) src
PIVOT
(
	SUM(src.Count)
	FOR src.ProductName IN (
		[KitKat], [5th Avenue], [Butterfinger], [Crunch], [Ghirardelli Intense Dark], [Godiva Chocolate and Almond],
		[Hershey's], [Hershey's Cookies & Cream], [Lindt Extra Creamy], [M&M's], [Peanut M&M's], [Snickers], [Twix]
	)
) piv
ORDER BY Total DESC;