var app = angular.module('custDataUploader', []);

app.controller('UploadController', ['$scope', function ($scope) {
    var status = {
        fileSelected: false,
        total: 0,
        bob: 'Uncle Bob'
    };


    function fileChosen($scope) {
        //$('#submitFile').removeAttr("disabled");
        $scope.status.fileSelected = true;
        $scope.status.total = 20;
        $scope.status.bob = "Hey";
    }

}]);