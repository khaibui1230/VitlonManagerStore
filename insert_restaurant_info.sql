-- Insert default restaurant information if none exists
IF NOT EXISTS (SELECT * FROM RestaurantInfo)
BEGIN
    INSERT INTO RestaurantInfo (
        Name, 
        Address, 
        Phone, 
        Email, 
        TaxId, 
        LogoUrl, 
        WelcomeMessage, 
        GoodbyeMessage
    )
    VALUES (
        N'Quán Hiển - Vịt Lộn - Cút lộn', 
        N'354 Lê Văn Thọ, phường 11, quận Gò Vấp, TP HCM', 
        N'0379665639', 
        N'vitlonhien@gmail.com', 
        N'123456789', 
        N'', 
        N'Cảm ơn quý khách đã sử dụng dịch vụ!', 
        N'Hẹn gặp lại quý khách lần sau!'
    );
END; 