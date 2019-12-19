﻿using DotNetty.Transport.Channels;
using System;

namespace Coldairarrow.DotNettySocket
{
    class CommonChannelHandler : SimpleChannelInboundHandler<object>
    {
        public CommonChannelHandler(IChannelEvent channelEvent)
        {
            _channelEvent = channelEvent;
        }
        IChannelEvent _channelEvent { get; }

        

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            _channelEvent.OnChannelReceive(ctx, msg);
        }


        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            base.ChannelRead(ctx, msg);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _channelEvent.OnChannelActive(context);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void Flush(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void Read(IChannelHandlerContext context)
        {
            base.Read(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _channelEvent.OnChannelInactive(context.Channel);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _channelEvent.OnException(context.Channel, exception);
        }
    }
}