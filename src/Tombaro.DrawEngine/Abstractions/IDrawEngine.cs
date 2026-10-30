using Tombaro.DrawEngine.Contracts;

namespace Tombaro.DrawEngine.Abstractions
{
    /// <summary>
    /// Defines a contract for executing deterministic draws over application-owned ticket objects.
    /// </summary>
    public interface IDrawEngine
    {
        /// <summary>
        /// Executes a draw and returns the original ticket objects together with their draw positions.
        /// </summary>
        DrawResult<TTicket> Execute<TTicket>(DrawRequest<TTicket> request)
            where TTicket : IDrawTicket;
    }
}
