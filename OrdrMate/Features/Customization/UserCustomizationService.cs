using Microsoft.EntityFrameworkCore.Metadata;
using OrdrMate.DTOs.Order;
using OrdrMate.Features.Customization.DTOs;
using OrdrMate.Managers;
using OrdrMate.Repositories;
using OrdrMate.Services;
using OrdrMate.Utils.Exceptions;

namespace OrdrMate.Features.Customization;

public class UserCustomizationService
{
    private readonly UserCustomizationRepo _repo;
    private readonly CustomizationService _customizationService;
    private readonly ITableRepo _tableRepo;
    public UserCustomizationService(UserCustomizationRepo repo, CustomizationService customizationService, ITableRepo tableRepo)
    {
        _repo = repo;
        _customizationService = customizationService;
        _tableRepo = tableRepo;
    }

    public async Task<UserCustomization?> GetUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));

        return await _repo.GetUserCustomization(userId, itemId);
    }

    public async Task<UserCustomization> CreateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));
        return await _repo.CreateUserCustomization(userCustomization);
    }

    public async Task<bool> UpdateUserCustomization(UserCustomization userCustomization)
    {
        ArgumentNullException.ThrowIfNull(userCustomization, nameof(userCustomization));
        return await _repo.UpdateUserCustomization(userCustomization);
    }

    public async Task<bool> DeleteUserCustomization(string userId, string itemId)
    {
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(itemId, nameof(itemId));
        return await _repo.DeleteUserCustomization(userId, itemId);
    }

    public async Task<bool> ValidateUserCustomization(OrderItemDto orderItem)
    {
        ArgumentNullException.ThrowIfNull(orderItem, nameof(orderItem));

        if (orderItem.Customizations == null || !orderItem.Customizations.Any())
        {
            throw new ArgumentNullException(nameof(orderItem.Customizations), "Order item customizations not found.");
        }

        var itemCustomizations = await _customizationService.GetItemCustomizations(orderItem.ItemId);
        if (itemCustomizations.Count() == 0)
        {
            return true;
        }

        foreach (var customization in itemCustomizations)
        {
            if (!orderItem.Customizations.ContainsKey(customization.Name))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<OrderItemsCustomizationResponseDto?> GetOrderCustomizationsAsync(string orderId)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new BadRequestException("Order ID cannot be null or empty.");
            }

            var customizations = await _repo.GetOrderCustomizationsAsync(orderId);

            var response = new OrderItemsCustomizationResponseDto
            {
                OrderId = orderId,
                Items = []
            };

            foreach (var customization in customizations)
            {
                response.Items.Add(new OrderItemCustomizationDto
                {
                    ItemId = customization.ItemId,
                    Customization = customization.CustomizationValues.ToDictionary()
                });
            }

            return response;
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"An error occurred while retrieving order customizations: {ex.Message}");
        }
    }

    public async Task<OrderItemsCustomizationResponseDto?> GetReservationCustomizations(string reservationId)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(reservationId))
            {
                throw new BadRequestException("Reservation ID cannot be null or empty.");
            }

            var orders = await _tableRepo.GetTableOrdersByReservationId(reservationId);
            if (orders == null)
            {
                throw new NotFoundException($"Reservation with ID {reservationId} not found.");
            }

            var response = new OrderItemsCustomizationResponseDto
            {
                OrderId = orders.First().Id,
                Items = []
            };

            foreach (var order in orders)
            {
                var customizations = await _repo.GetOrderCustomizationsAsync(order.Id);
                if (customizations == null || !customizations.Any())
                {
                    throw new NotFoundException($"No orders found for reservation ID {reservationId}.");
                }

                foreach (var customization in customizations)
                {
                    response.Items.Add(new OrderItemCustomizationDto
                    {
                        ItemId = customization.ItemId,
                        Customization = customization.CustomizationValues.ToDictionary()
                    });
                }
            }

            return response;
        }
        catch (OException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException($"An error occurred while retrieving reservation customizations: {ex.Message}");
        }
    }


}