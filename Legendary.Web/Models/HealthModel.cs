// <copyright file="UserModel.cs" company="Legendary">
//  Copyright © 2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Web.Models
{
    using System.Collections.Generic;

    public class HealthModel
    {
        public List<string> Messages { get; set; }

        public HealthModel(List<string> messages)
        {
            this.Messages = messages;
        }
    }
}

