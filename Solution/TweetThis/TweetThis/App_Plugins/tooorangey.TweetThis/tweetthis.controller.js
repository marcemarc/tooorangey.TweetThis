angular.module("umbraco").controller("tooorangey.TweetThisController", function ($scope, $http, entityResource, mediaHelper, navigationService, notificationsService, $location) {
    
    var vm = this;
    //content to tweet
    vm.content = {
        url:'',
        title:'',
        id: 0,
        blogTitle:'',
        blogUrl: '',
        tweetTemplate: ''
    };
    //status of tweet panel and twittering
    vm.status = {
        showTweetForm: false,
        readyToTweet: false,
        isTweeting: false,
        showStatusPanel: false,
        tweetMessage: '',
        charLimit: 140,
        messageStatus: '',
        Twitter_short_url_length: 23,
        tooManyCharacters: false,
        twitterAccount: {name:'',avatar:'',screenName:'',accountRegistered:false}
    };

    function init() {
        // make a call to twitter api to determine: Twitter_short_url_length
        //If you are not using the opt-in features, only links shorter in length than a t.co URL will be wrapped by t.co. All links t.co-length or longer should be considered as t.co’s maximum length. For example, if help/configuration reports 20 characters as the maximum length, and a user posts a link that is 125 characters long, it should be considered as 20 characters long instead. If they post a link that is 18 characters long, it’s still only 18 characters long.
        //hard coded to 23 for now       

        vm.status.twitterAccount.accountRegistered = false;
        // get current content item
        var dialogOptions = $scope.dialogOptions;
        var currentContentItem = dialogOptions.currentNode;
        vm.content.id = parseInt(currentContentItem.id);
        console.log(currentContentItem);
        //check if twitter account setup ok, if not error panel is shown by default
        $http.get('/umbraco/backoffice/api/tweetthisapi/GetTwitterAccount', { params: { id: currentContentItem.id } }).then(function (response) {
            if (response != null && response.data.screenName.length> 0) {
                vm.status.twitterAccount.screenName = response.data.screenName;
                vm.status.twitterAccount.name = response.data.name;
                vm.status.twitterAccount.avatar = response.data.avatar;
                vm.status.twitterAccount.accountRegistered = true;
            }
        });
        // use entity resource to pull back it's url
        entityResource.getById(currentContentItem.id, "Document").then(function (contentEntity) {
            console.log(contentEntity);
            //get blog title here?
            vm.content.title = contentEntity.name;
            //this is 7.6+ only
            entityResource.getUrl(currentContentItem.id,"Document").then(function(response){
            // need to replace with an api endpoint in the TweetThisController if < 7.6
            //that will return the url of a specific page id...
            //eg: $http.get('/umbraco/backoffice/api/tweetthisapi/GetByUrl', { params: { id: contentEntity.id } }).then(function (response) {
                vm.content.url = response;
                if (vm.content.Url != '') {
                    // get blog Info
                    $http.get("/umbraco/backoffice/api/tweetthisapi/getbloginfofromblogpost", { params: { id: currentContentItem.id } }).then(function (response) {
                        console.log(response);
                        vm.content.tweetTemplate = response.data.TweetMessageTemplate;
                        vm.content.blogTitle = response.data.BlogTitle;
                        vm.content.blogUrl = response.data.BlogUrl; 

                    // if ok show the twitter form
                    vm.status.tweetMessage = vm.content.tweetTemplate;
                    vm.status.showTweetForm = true;
                    vm.status.readyToTweet = true;
                    limitChars();
                    });
                }		
            });
        });		
    };

    vm.limitChars = limitChars
    vm.tweetThis = tweetThis;
  //calculates how many characters left.... hopefully
    function limitChars(){
        //twitter text will include links
        //these will be shortened, to 23 chars, need to reflect this in number of characters left
        var tweetParts = vm.status.tweetMessage.split(" ");
        var characterCount = vm.status.tweetMessage.length;
        var noOfSpaces = tweetParts.length;
        for (var i = 0, l = tweetParts.length; i < l; i++) {
            //console.log(tweetParts[i]);
            if (tweetParts[i].toLowerCase().startsWith("http") && tweetParts[i].length > vm.status.Twitter_short_url_length) {
                //link will be shortened
                var shortenAmount = tweetParts[i].length - vm.status.Twitter_short_url_length;
                characterCount = characterCount - shortenAmount;
            }
        }
        //status message
        if (characterCount > vm.status.charLimit) {
            vm.status.readyToTweet = false;
            vm.status.tooManyCharacters = true;
            vm.status.messageStatus = "You cannot write more than " + vm.status.charLimit  + " characters! (" + (vm.status.charLimit - characterCount) + ")";
        } else {   
            vm.status.messageStatus = "You have " + (vm.status.charLimit - characterCount) + 
                 " characters left.";
            vm.status.readyToTweet = true;
            vm.status.tooManyCharacters = false;
        }
    };
    //check if it's ok to do the tweet, then send details to api endpoint
    function tweetThis(){
        vm.status.isTweeting= true;
        // do the tweeting
        vm.status.isTweeting = false;
        vm.status.readyToTweet = false;
    //send the tweet
        $http.post("/umbraco/backoffice/api/tweetthisapi/tweetthis", JSON.stringify({ ContentId: parseInt(vm.content.id), Message:  vm.status.tweetMessage }),
                  {
                      headers: {
                          'Content-Type': 'application/json'
                      }
                  }).then(function (response) {
                       console.log(response);
                      // close the slide out box
                       navigationService.hideDialog();
                       notificationsService.remove(0);
                       notificationsService.success("Tweet Sent", "Your tweet about " + vm.content.title + " was tweeted!");
                                          

                  }, function (response) {
                      //console.log(response);
                      //notify errors
                      navigationService.hideDialog();
                      notificationsService.remove(0);
                      notificationsService.error("Error Sending Tweet", response.data.Message);
                  });  
    }  
    init();
});