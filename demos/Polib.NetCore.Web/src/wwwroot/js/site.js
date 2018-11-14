// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function (ng) {
    ng.module('polib', [])
        .service('CatalogService', ['$http', function ($http) {

            this.getCatalog = function (id) {
                return $http.get(`/translations/getcatalog/${id}`);
            };

            this.updateCatalog = function (catalog) {
                return $http.post('/translations/updatecatalog', catalog);
            };
        }])
        .controller('CatalogController', ['$scope', 'CatalogService', function ($scope, CatalogService) {
            console.log('Angular translation catalog controller set up correctly.');

            const me = $scope;
            var __RequestVerificationToken = '';

            /** me.catalog = {
                    id: '',
                    pluralCount: 0,
                    items: {
                        uniqueKey: '',
                        context: '',
                        singular: '',
                        plural: '',
                        translations: [''],
                        extractedComments: '',
                        flags: [''],
                        references: [''],
                        translatorComments: ''
                    }
                }*/
            me.catalog = {};

            me.load = function (id) {
                var token = $('input[name=__RequestVerificationToken]').val();
                __RequestVerificationToken = token;
                CatalogService.getCatalog(id).success(function (result) {
                    me.catalog = result;
                });
            };

            me.truncate = function (s, maxlen) {
                maxlen = maxlen || 110;
                if (!s || s.length <= maxlen) return s;
                return s.substring(0, maxlen) + '...';
            };

            /**
             * Show plural form translations for the specified entry.
             * @param {any} entry The translation entry.
             * @returns {array} An array of translation entries' indices.
             */
            me.pluralIndices = function (entry) {
                if (!entry || entry.translations.length < 2) return [];

                const items = [],
                    translations = entry.translations;

                // push indices of translated entries
                for (let i = 1; i < translations.length; i++) {
                    items.push(i);
                }

                // eventually make place for more plural forms than actually translated
                for (let i = translations.length; i < me.catalog.pluralCount; i++) {
                    items.push(i);
                }

                return items;
            };

            me.show = function (t) {
                me.current = t;
            };

            me.selected = function (t) {
                return me.current && me.current.row === t.row;
            };

            me.isplural = function (t) {
                t = t || me.current;
                return t && t.translations.length > 1;
            };

            me.update = function () {
                if (!me.current) return;
                me.catalog.__RequestVerificationToken = __RequestVerificationToken;
                CatalogService.updateCatalog(me.catalog)
                    .success(function (result) {
                        console.log(result);
                });
            };

            me.hasTranslation = function (t) {
                return t && t.translations.length > 0 && t.translations[0] !== '';
            };
        }]);
})(angular);
