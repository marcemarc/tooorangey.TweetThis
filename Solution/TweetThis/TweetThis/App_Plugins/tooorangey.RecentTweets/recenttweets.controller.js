angular.module("umbraco").controller("Moriyama.RecentTweets", function ($scope, $http, mediaResource, editorState, notificationsService) {
    var vm = this;
     // stolen from view: '/umbraco/views/propertyeditors/imagecropper/imagecropper.html',
    $scope.model.hideLabel = $scope.model.config.hideLabel == 1;

    function init() {
        //get current id of 'blog'
        var currentBlogId= editorState.current.id;
        //make request to endpoint
        $http.get("/umbraco/backoffice/api/tweetthisapi/GetRecentTweets", {params: {blogid: currentBlogId }}).then(
                    function (response) {
                        console.log(response);
                        vm.status.tweets = response.data.Body;
                        //list out tweets 
                    });




        };
       


    vm.status = {
        loadingTweets: true,
        tweets: []
    }
  


  
    init();

});