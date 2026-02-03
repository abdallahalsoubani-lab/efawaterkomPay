namespace DirectPayGateway.Core.Constants;

public static class DirectPayErrorCodes
{
    public static readonly Dictionary<int, (string En, string Ar)> Messages = new()
    {
        { 1, ("Success", "نجاح") },
        { 2, ("Wrong Biller Transaction No", "رقم معاملة خاطئ") },
        { 3, ("Wrong Biller Code", "رمز المفوتر خاطئ") },
        { 4, ("Wrong Service Code", "رمز الخدمة خاطئ") },
        { 5, ("Wrong Prepaid Category Code", "رمز فئة الدفع المسبق خاطئ") },
        { 6, ("Wrong Amount", "المبلغ خاطئ") },
        { 7, ("Wrong Customer Email", "البريد الإلكتروني خاطئ") },
        { 8, ("Wrong Call Back URL", "رابط الاستجابة خاطئ") },
        { 9, ("Parsing Error", "خطأ في تحليل البيانات") },
        { 10, ("Internal Error", "خطأ داخلي") },
        { 11, ("Unable to process your payment, please try again later", "تعذر معالجة الدفع، يرجى المحاولة لاحقاً") },
        { 12, ("Payment transaction is canceled by customer", "تم إلغاء العملية من قبل العميل") },
        { 13, ("Insufficient balance", "رصيد غير كافٍ") },
        { 14, ("No registered mobile number, please refer to your bank", "لا يوجد رقم جوال مسجل، يرجى مراجعة البنك") },
        { 15, ("Unable to process your payment, please get back to your bank", "تعذر معالجة الدفع، يرجى مراجعة البنك") },
        { 16, ("The entered OTP is incorrect", "رمز التحقق غير صحيح") },
        { 17, ("The entered OTP is expired", "رمز التحقق منتهي الصلاحية") },
        { 18, ("You reached the maximum limit to request a new OTP", "تجاوزت الحد الأقصى لطلب رمز تحقق جديد") },
        { 19, ("Bank is not available", "البنك غير متاح") },
        { 20, ("Invalid Token", "رمز غير صالح") }
    };

    public static string GetMessage(int code, string language = "EN")
    {
        if (Messages.TryGetValue(code, out var message))
        {
            return language.ToUpperInvariant() == "AR" ? message.Ar : message.En;
        }
        return language.ToUpperInvariant() == "AR" ? "خطأ غير معروف" : "Unknown error";
    }

    public static bool IsSuccess(int code) => code == 1;

    public static bool IsProcessing(int paymentStatus) => paymentStatus == 2;

    public static bool IsFailed(int paymentStatus) => paymentStatus == 3;
}
