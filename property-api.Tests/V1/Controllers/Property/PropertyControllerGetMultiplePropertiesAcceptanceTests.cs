using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using property_api.V1.Controllers;
using property_api.V1.UseCase;
using property_api.V1.Domain;
using Moq;
using Bogus;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using property_api.V1.UseCase.GetMultipleProperties;
using FluentAssertions;
using property_api.V1.Validation;
using property_api.V1.Gateways.GetMultipleProperties;
using property_api.V1.Factory;
using property_api.V1.Helpers;
using property_api.V1.Gateways;
using UnitTests.V1.Helpers;
using property_api.V1.UseCase.GetMultipleProperties.Impl;
using property_api.V1.UseCase.GetMultipleProperties.Boundaries;
using Newtonsoft.Json;

namespace UnitTests.V1.Controllers
{
    [TestFixture]
    public class PropertyControllerGetMultiplePropertiesAcceptanceTests : DbTest
    {
        private PropertyController _classUnderTest;
        private Mock<ILogger<PropertyController>> _mockLogger;
        private IGetMultiplePropertiesUseCase _getMultiplePropertiesUseCase;
        private IGetMultiplePropertiesValidator _getMultiplePropertiesValidator;
        private PropertyFactory _factory;
        private IGetMultiplePropertiesGateway _getMultiplePropertiesGateway;
        private readonly PropertyTestHelper _uhPropertyHelper = new PropertyTestHelper();

        [SetUp]
        public void SetUp()
        {
            var config = PropertyHelper.ConfigureMapper();
            _factory = new PropertyFactory(config.CreateMapper());

            _mockLogger = new Mock<ILogger<PropertyController>>();
            _getMultiplePropertiesValidator = new GetMultiplePropertiesValidator();

            _getMultiplePropertiesGateway = new PropertyGateway(_uhContext, _factory);
            _getMultiplePropertiesUseCase = new GetMultiplePropertiesUseCase(_getMultiplePropertiesGateway);
            _classUnderTest = new PropertyController(null, _mockLogger.Object, _getMultiplePropertiesUseCase, _getMultiplePropertiesValidator);
        }

        [TestCase("1", "2")]
        [TestCase("3", "4")]
        public void GivenListOfPropRefsAreProvided_WhenEndpointIsCalled_ThenItShouldReturnTheMultiplePropertiesSpecified(string propertyReference, string propertyReference2)
        {
            //arrange
            var property1 = _uhPropertyHelper.GenerateUhProperty();
            property1.PropRef = propertyReference;
            var property2 = _uhPropertyHelper.GenerateUhProperty();
            property2.PropRef = propertyReference2;

            _uhContext.UhPropertys.Add(property1);
            _uhContext.UhPropertys.Add(property2);
            _uhContext.SaveChanges();

            List<string> propertyReferences = new List<string> { propertyReference, propertyReference2 };

            var expectedJson = JsonConvert.SerializeObject(
                new GetMultiplePropertiesUseCaseResponse
                {
                    Properties = new List<Property>
                    {
                        _factory.FromUHProperty(property1),
                        _factory.FromUHProperty(property2)
                    }
                }
            );
            //act
            var actual = _classUnderTest.GetMultipleByReference(propertyReferences);
            //assert
            actual.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)actual;
            var response = (GetMultiplePropertiesUseCaseResponse)okObjectResult.Value;
            response.Should().NotBeNull();
            response.Properties.Should().NotBeNullOrEmpty();

            response.Properties.Should().BeOfType<List<Property>>();
            Assert.AreSame(propertyReference, response.Properties[0].PropRef);
            Assert.AreSame(propertyReference2, response.Properties[1].PropRef);

            var actualJson = JsonConvert.SerializeObject(response);
            expectedJson.Should().BeEquivalentTo(actualJson);
        }
    }
}
