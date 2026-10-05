using System;
using System.Collections.Generic;

namespace MobileServiceSite.Models;

public partial class Order
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int DeviceId { get; set; }

    /// <summary>Описание работы, которую просит клиент</summary>
    public string Description { get; set; } = null!;

    /// <summary>Цена заказа. null — админ ещё не установил цену</summary>
    public int? Price { get; set; }

    /// <summary>Статус заказа, см. OrderStatus</summary>
    public string Status { get; set; } = OrderStatus.Created;

    public DateTime CreatedAt { get; set; }

    /// <summary>Когда админ установил цену</summary>
    public DateTime? PriceSetAt { get; set; }

    /// <summary>Когда клиент оплатил заказ</summary>
    public DateTime? PaidAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Device Device { get; set; } = null!;
}

/// <summary>
/// Жизненный цикл заказа:
/// Created (клиент создал) → AwaitingPayment (админ установил цену)
/// → Paid (клиент оплатил, админу пришло уведомление)
/// → InProgress (админ начал работу) → Completed
/// </summary>
public static class OrderStatus
{
    public const string Created = "Created";
    public const string AwaitingPayment = "AwaitingPayment";
    public const string Paid = "Paid";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";

    public static string Display(string? status) => status switch
    {
        Created => "Создан",
        AwaitingPayment => "Ожидает оплаты",
        Paid => "Оплачен",
        InProgress => "В работе",
        Completed => "Выполнен",
        _ => status ?? "Неизвестно"
    };
}
